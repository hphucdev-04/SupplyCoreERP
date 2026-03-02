export class CodeGeneratorUtil {

  static generate(name: string, prefix: string): string {
    if (!name) {
      return this.randomCode(prefix);
    }

    const normalized = name
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd').replace(/Đ/g, 'D')
      .replace(/[^a-zA-Z0-9 ]/g, '')
      .trim();

    const initials = normalized
      .split(/\s+/)
      .map(word => word.charAt(0))
      .join('')
      .toUpperCase()
      .substring(0, 8);

    return this.randomCode(prefix, initials);
  }

  private static randomCode(prefix: string, initials?: string): string {
    const array = new Uint32Array(1);
    crypto.getRandomValues(array);
    const cryptoHex = array[0]
      .toString(16)
      .toUpperCase()
      .padStart(8, '0');

    return initials
      ? `${prefix}_${initials}_${cryptoHex}`
      : `${prefix}_${cryptoHex}`;
  }
}