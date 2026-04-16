export function enumName(enumType: object, value: number, i18nPrefix?: string): string {
  const name = (enumType as any)[value] ?? '';
  return i18nPrefix ? `${i18nPrefix}.${name}` : name;
}