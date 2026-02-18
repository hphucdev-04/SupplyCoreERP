import type { EntityDto } from '@abp/ng.core';
import type { CurrencyType } from '../../enums/currency-type.enum';

export interface CreateUpdateProductPriceDto {
  priceListId: string;
  productId: string;
  unitId: string;
  price: number;
  minQuantity?: number;
}

export interface PriceListDto extends EntityDto<string> {
  code?: string;
  name?: string;
  currency?: CurrencyType;
  isBase?: boolean;
}

export interface ProductPriceDto extends EntityDto<string> {
  priceListId?: string;
  priceListName?: string;
  priceListCode?: string;
  currency?: CurrencyType;
  unitId?: string;
  unitName?: string;
  price?: number;
  minQuantity?: number;
}
