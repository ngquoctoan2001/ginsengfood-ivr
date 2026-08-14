using System.Reflection;
using System.Reflection.Emit;

namespace Ivr.IntegrationTests;

internal static class NoEgressIlGuard
{
    private static readonly Dictionary<short, OpCode> OpCodesByValue =
        typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opCode => opCode.Value);

    public static void AssertNoCallTargets(Type type)
    {
        IEnumerable<MethodBase> methods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Cast<MethodBase>()
            .Concat(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        foreach (MethodBase method in methods)
        {
            byte[]? il = method.GetMethodBody()?.GetILAsByteArray();
            if (il is null)
            {
                continue;
            }

            for (int offset = 0; offset < il.Length;)
            {
                short value = il[offset++] == 0xFE
                    ? unchecked((short)(0xFE00 | il[offset++]))
                    : il[offset - 1];
                OpCode opCode = OpCodesByValue[value];
                if (opCode.OperandType is OperandType.InlineField
                    or OperandType.InlineMethod
                    or OperandType.InlineTok
                    or OperandType.InlineType)
                {
                    MemberInfo member = method.Module.ResolveMember(
                        BitConverter.ToInt32(il, offset),
                        type.GetGenericArguments(),
                        method is MethodInfo info ? info.GetGenericArguments() : null)!;
                    string? memberNamespace = member switch
                    {
                        Type memberType => memberType.Namespace,
                        _ => member.DeclaringType?.Namespace,
                    };
                    Assert.False(
                        memberNamespace?.StartsWith("System.Net", StringComparison.Ordinal) == true
                        || memberNamespace?.StartsWith("System.IO.Ports", StringComparison.Ordinal) == true,
                        $"{method.Name} references egress member {member.DeclaringType?.FullName}.{member.Name}");
                }

                offset += OperandSize(opCode.OperandType, il, offset);
            }
        }
    }

    private static int OperandSize(OperandType operandType, byte[] il, int offset) =>
        operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI
                or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI
                or OperandType.InlineMethod or OperandType.InlineSig
                or OperandType.InlineString or OperandType.InlineTok
                or OperandType.InlineType or OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => checked(4 + (BitConverter.ToInt32(il, offset) * 4)),
            _ => throw new InvalidOperationException($"Unsupported IL operand {operandType}."),
        };
}
