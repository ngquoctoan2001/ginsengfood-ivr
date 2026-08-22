namespace Ivr.Domain.Speech;

/// <summary>
/// The three Vietnamese regions a delivery address can map to, one per IVR voice.
/// <para>
/// Owner decision <c>OD-VOICE-02</c> (2026-08-22): the split is taken purely from the
/// provincial-level unit, with no ward-level exception and no demographic override. The
/// Central Highlands (Gia Lai, Đắk Lắk, Lâm Đồng, and the former Kon Tum now inside Quảng
/// Ngãi) belong to <see cref="Central"/> under the standard "Trung Bộ và Tây Nguyên"
/// convention.
/// </para>
/// </summary>
public enum VietnamRegion
{
    North,
    Central,
    South,
}
