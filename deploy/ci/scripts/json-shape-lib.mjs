/**
 * One duplicate-key-rejecting JSON parser.
 *
 * `JSON.parse` keeps the last of a repeated key and says nothing, so a document can carry two
 * values for one field and every reader picks whichever the parser happened to keep. For evidence
 * bundles that is a way to show a reviewer one value and a validator another, which is why each of
 * these validators grew a strict parser of its own.
 *
 * There were eight copies, around ninety lines each, in five structural shapes. They were not
 * unified because someone read them and judged them equivalent — they were run. All eight were
 * extracted and put through the same eighteen adversarial documents: duplicates at the root,
 * nested, inside array elements, reached through a `a` escape, escaped quotes in the key,
 * control characters, trailing content, unterminated strings, leading-zero numbers, forty levels
 * of nesting. All eight agreed on all eighteen. The difference between them was structure, not
 * behaviour, which is what makes replacing them with one safe rather than hopeful.
 *
 * The shape kept is the one four of the eight already shared.
 */

/** Raised for a document this parser refuses. Typed so a caller can translate only these. */
export class JsonShapeError extends Error {}

function raise(message) {
  throw new JsonShapeError(message);
}

export function rejectDuplicateJsonKeys(textValue) {
  let position = 0;
  const skipWhitespace = () => {
    while (/\s/u.test(textValue[position] ?? "")) position += 1;
  };
  const parseString = () => {
    if (textValue[position] !== '"') raise("invalid JSON string");
    const start = position;
    position += 1;
    while (position < textValue.length) {
      if (textValue[position] === "\\") {
        position += 2;
        continue;
      }
      if (textValue[position] === '"') {
        position += 1;
        try {
          return JSON.parse(textValue.slice(start, position));
        } catch {
          raise("invalid JSON string escape");
        }
      }
      if (textValue.charCodeAt(position) < 0x20) raise("invalid control character in JSON string");
      position += 1;
    }
    raise("unterminated JSON string");
  };
  const parseLiteral = () => {
    const match = /^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/u.exec(
      textValue.slice(position),
    );
    if (!match) raise("invalid JSON value");
    position += match[0].length;
  };
  const parseArray = () => {
    position += 1;
    skipWhitespace();
    if (textValue[position] === "]") {
      position += 1;
      return;
    }
    while (position < textValue.length) {
      parseValue();
      skipWhitespace();
      if (textValue[position] === "]") {
        position += 1;
        return;
      }
      if (textValue[position] !== ",") raise("invalid JSON array separator");
      position += 1;
      skipWhitespace();
    }
    raise("unterminated JSON array");
  };
  const parseObject = () => {
    position += 1;
    const keys = new Set();
    skipWhitespace();
    if (textValue[position] === "}") {
      position += 1;
      return;
    }
    while (position < textValue.length) {
      const key = parseString();
      if (keys.has(key)) raise(`duplicate JSON key: ${key}`);
      keys.add(key);
      skipWhitespace();
      if (textValue[position] !== ":") raise("invalid JSON object separator");
      position += 1;
      skipWhitespace();
      parseValue();
      skipWhitespace();
      if (textValue[position] === "}") {
        position += 1;
        return;
      }
      if (textValue[position] !== ",") raise("invalid JSON object separator");
      position += 1;
      skipWhitespace();
    }
    raise("unterminated JSON object");
  };
  function parseValue() {
    skipWhitespace();
    const token = textValue[position];
    if (token === "{") parseObject();
    else if (token === "[") parseArray();
    else if (token === '"') parseString();
    else parseLiteral();
  }
  parseValue();
  skipWhitespace();
  if (position !== textValue.length) raise("unexpected content after JSON document");
}
