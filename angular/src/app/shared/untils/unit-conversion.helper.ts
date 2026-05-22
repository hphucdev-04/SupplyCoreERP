export interface UnitConversionProduct {
  baseUnitId?: string;
  units?: {
    unitId?: string;
    conversionFactor?: number;
  }[];
}

export class UnitConversionHelper {
  /**
   * Quy đổi số lượng từ một đơn vị bất kỳ về số lượng theo đơn vị gốc (Base Quantity)
   */
  static convertToBaseQuantity(
    product: UnitConversionProduct | null | undefined,
    unitId: string | null | undefined,
    quantity: number,
  ): number {
    if (!product || !unitId || quantity === undefined || quantity === null) {
      return quantity || 0;
    }

    if (product.baseUnitId === unitId) {
      return quantity;
    }

    const conversionFactor = this.getConversionFactor(product, unitId);
    return quantity * conversionFactor;
  }

  /**
   * Quy đổi số lượng từ số lượng đơn vị gốc (Base Quantity) sang đơn vị chỉ định
   */
  static convertFromBaseQuantity(
    product: UnitConversionProduct | null | undefined,
    unitId: string | null | undefined,
    baseQuantity: number,
    decimals?: number,
  ): number {
    if (!product || !unitId || baseQuantity === undefined || baseQuantity === null) {
      return baseQuantity || 0;
    }

    if (product.baseUnitId === unitId) {
      return baseQuantity;
    }

    const conversionFactor = this.getConversionFactor(product, unitId);
    if (conversionFactor === 0) {
      return 0;
    }

    const result = baseQuantity / conversionFactor;

    if (decimals !== undefined && decimals !== null) {
      return this.round(result, decimals);
    }

    return result;
  }

  /**
   * Lấy hệ số quy đổi của một đơn vị cụ thể
   */
  static getConversionFactor(
    product: UnitConversionProduct | null | undefined,
    unitId: string | null | undefined,
  ): number {
    if (!product || !unitId) {
      return 1;
    }

    if (product.baseUnitId === unitId) {
      return 1;
    }

    const unit = product.units?.find(u => u.unitId === unitId);
    return unit && unit.conversionFactor !== undefined && unit.conversionFactor !== null
      ? unit.conversionFactor
      : 1;
  }

  /**
   * Hàm làm tròn số chuẩn thập phân (tránh sai số dấu phẩy động)
   */
  private static round(value: number, decimals: number): number {
    const factor = Math.pow(10, decimals);
    return Math.round((value + Number.EPSILON) * factor) / factor;
  }
}
