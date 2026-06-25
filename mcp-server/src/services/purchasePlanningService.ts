import { queryDb } from "../db.js";
import { PurchasePlanningInput, PurchasePlanningResult, PlanningLine } from "./purchasePlanningTypes.js";

interface ProductWarehouseRow {
  ProductId: string;
  ProductCode: string;
  ProductName: string;
  WarehouseId: string;
  WarehouseCode: string;
  WarehouseName: string;
}

interface QuantityRow {
  ProductId: string;
  WarehouseId: string;
  OnHandQuantity?: string | number;
  LockedQuantity?: string | number;
  AvailableQuantity?: string | number;
  ForecastDemand?: string | number;
  IncomingQuantity?: string | number;
  RequisitionBacklogQuantity?: string | number;
}

interface SupplierOptionRow {
  ProductId: string;
  SupplierId: string;
  SupplierName: string;
  ConditionUnitId: string;
  ConditionUnitName: string;
  ConversionFactor: string | number;
  LeadTimeDays: string | number;
  IsPreferred: boolean;
  StandardPrice: string | number;
  LastPurchasePrice: string | number;
  MinOrderQuantity: string | number;
}

interface SupplierOption {
  supplierId: string;
  supplierName: string;
  conditionUnitId: string;
  conditionUnitName: string;
  conversionFactor: number;
  leadTimeDays: number;
  isPreferred: boolean;
  standardPrice: number;
  lastPurchasePrice: number;
  minOrderQuantity: number;
}

const toNumber = (value: string | number | null | undefined): number => {
  if (value === null || value === undefined) {
    return 0;
  }

  if (typeof value === "number") {
    return value;
  }

  return Number(value);
};

const formatDate = (value: Date): string => value.toISOString().slice(0, 10);

const buildProductWarehouseKey = (productId: string, warehouseId: string): string => `${productId}::${warehouseId}`;

const roundUpToIncrement = (value: number, increment: number): number => {
  if (value <= 0) {
    return 0;
  }

  if (increment <= 0) {
    return Math.ceil(value);
  }

  return Math.ceil(value / increment) * increment;
};

const calculateForecastWindow = (targetYear: number, targetMonth: number): { historyStart: string; historyEnd: string; daysInMonth: number } => {
  const historyStart = new Date(Date.UTC(targetYear, targetMonth - 4, 1));
  const historyEnd = new Date(Date.UTC(targetYear, targetMonth - 1, 0));
  const daysInMonth = new Date(targetYear, targetMonth, 0).getDate();

  return {
    historyStart: formatDate(historyStart),
    historyEnd: formatDate(historyEnd),
    daysInMonth
  };
};

const fetchProductWarehouseUniverse = async (
  warehouseIds: string[] | undefined,
  productIds: string[] | undefined,
  historyStart: string,
  historyEnd: string
): Promise<ProductWarehouseRow[]> => {
  let query = `
    WITH universe AS (
      SELECT DISTINCT
        ib."ProductId",
        ib."WarehouseId"
      FROM "AppInventoryBalances" ib
      UNION
      SELECT DISTINCT
        sol."ProductId",
        so."WarehouseId"
      FROM "AppSalesOrderLines" sol
      INNER JOIN "AppSalesOrders" so ON so."Id" = sol."SalesOrderId"
      WHERE so."IsDeleted" = false
        AND so."OrderDate" >= $1
        AND so."OrderDate" < ($2::date + INTERVAL '1 day')
      UNION
      SELECT DISTINCT
        pol."ProductId",
        po."WarehouseId"
      FROM "AppPurchaseOrderLines" pol
      INNER JOIN "AppPurchaseOrders" po ON po."Id" = pol."PurchaseOrderId"
      WHERE po."IsDeleted" = false
        AND pol."Quantity" > pol."ReceivedQuantity"
      UNION
      SELECT DISTINCT
        prl."ProductId",
        pr."WarehouseId"
      FROM "AppPurchaseRequisitionLines" prl
      INNER JOIN "AppPurchaseRequisitions" pr ON pr."Id" = prl."PurchaseRequisitionId"
      WHERE pr."IsDeleted" = false
        AND prl."Quantity" > prl."OrderedQuantity"
    )
    SELECT
      u."ProductId" AS "ProductId",
      p."Code" AS "ProductCode",
      p."Name" AS "ProductName",
      u."WarehouseId" AS "WarehouseId",
      w."Code" AS "WarehouseCode",
      w."Name" AS "WarehouseName"
    FROM universe u
    INNER JOIN "AppProducts" p ON p."Id" = u."ProductId"
    INNER JOIN "AppWarehouses" w ON w."Id" = u."WarehouseId"
    WHERE p."IsDeleted" = false
      AND w."IsDeleted" = false
  `;
  const params: Array<string | string[]> = [historyStart, historyEnd];

  if (warehouseIds && warehouseIds.length > 0) {
    params.push(warehouseIds);
    query += ` AND u."WarehouseId" = ANY($${params.length})`;
  }

  if (productIds && productIds.length > 0) {
    params.push(productIds);
    query += ` AND u."ProductId" = ANY($${params.length})`;
  }

  query += ` ORDER BY w."Name" ASC, p."Name" ASC`;

  return await queryDb(query, params) as ProductWarehouseRow[];
};

const fetchInventorySnapshot = async (
  warehouseIds: string[] | undefined,
  productIds: string[] | undefined
): Promise<Map<string, QuantityRow>> => {
  let query = `
    SELECT
      ib."ProductId" AS "ProductId",
      ib."WarehouseId" AS "WarehouseId",
      SUM(ib."Quantity") AS "OnHandQuantity",
      SUM(ib."LockedQuantity") AS "LockedQuantity",
      SUM(ib."Quantity" - ib."LockedQuantity") AS "AvailableQuantity"
    FROM "AppInventoryBalances" ib
    WHERE 1 = 1
  `;
  const params: Array<string[]> = [];

  if (warehouseIds && warehouseIds.length > 0) {
    params.push(warehouseIds);
    query += ` AND ib."WarehouseId" = ANY($${params.length})`;
  }

  if (productIds && productIds.length > 0) {
    params.push(productIds);
    query += ` AND ib."ProductId" = ANY($${params.length})`;
  }

  query += ` GROUP BY ib."ProductId", ib."WarehouseId"`;

  const rows = await queryDb(query, params) as QuantityRow[];
  const map = new Map<string, QuantityRow>();

  for (const row of rows) {
    map.set(buildProductWarehouseKey(row.ProductId, row.WarehouseId), row);
  }

  return map;
};

const fetchForecastDemand = async (
  warehouseIds: string[] | undefined,
  productIds: string[] | undefined,
  historyStart: string,
  historyEnd: string
): Promise<Map<string, QuantityRow>> => {
  let query = `
    SELECT
      monthly_data."ProductId" AS "ProductId",
      monthly_data."WarehouseId" AS "WarehouseId",
      AVG(monthly_data."DeliveredQuantity") AS "ForecastDemand"
    FROM (
      SELECT
        sol."ProductId",
        so."WarehouseId",
        DATE_TRUNC('month', so."OrderDate") AS "OrderMonth",
        SUM(sol."DeliveredQuantity") AS "DeliveredQuantity"
      FROM "AppSalesOrderLines" sol
      INNER JOIN "AppSalesOrders" so ON so."Id" = sol."SalesOrderId"
      WHERE so."IsDeleted" = false
        AND so."OrderDate" >= $1
        AND so."OrderDate" < ($2::date + INTERVAL '1 day')
      GROUP BY
        sol."ProductId",
        so."WarehouseId",
        DATE_TRUNC('month', so."OrderDate")
    ) monthly_data
    WHERE 1 = 1
  `;
  const params: Array<string | string[]> = [historyStart, historyEnd];

  if (warehouseIds && warehouseIds.length > 0) {
    params.push(warehouseIds);
    query += ` AND monthly_data."WarehouseId" = ANY($${params.length})`;
  }

  if (productIds && productIds.length > 0) {
    params.push(productIds);
    query += ` AND monthly_data."ProductId" = ANY($${params.length})`;
  }

  query += ` GROUP BY monthly_data."ProductId", monthly_data."WarehouseId"`;

  const rows = await queryDb(query, params) as QuantityRow[];
  const map = new Map<string, QuantityRow>();

  for (const row of rows) {
    map.set(buildProductWarehouseKey(row.ProductId, row.WarehouseId), row);
  }

  return map;
};

const fetchInboundPipeline = async (
  warehouseIds: string[] | undefined,
  productIds: string[] | undefined
): Promise<Map<string, QuantityRow>> => {
  let query = `
    SELECT
      pol."ProductId" AS "ProductId",
      po."WarehouseId" AS "WarehouseId",
      SUM(pol."Quantity" - pol."ReceivedQuantity") AS "IncomingQuantity"
    FROM "AppPurchaseOrderLines" pol
    INNER JOIN "AppPurchaseOrders" po ON po."Id" = pol."PurchaseOrderId"
    WHERE po."IsDeleted" = false
      AND pol."Quantity" > pol."ReceivedQuantity"
  `;
  const params: Array<string[]> = [];

  if (warehouseIds && warehouseIds.length > 0) {
    params.push(warehouseIds);
    query += ` AND po."WarehouseId" = ANY($${params.length})`;
  }

  if (productIds && productIds.length > 0) {
    params.push(productIds);
    query += ` AND pol."ProductId" = ANY($${params.length})`;
  }

  query += ` GROUP BY pol."ProductId", po."WarehouseId"`;

  const rows = await queryDb(query, params) as QuantityRow[];
  const map = new Map<string, QuantityRow>();

  for (const row of rows) {
    map.set(buildProductWarehouseKey(row.ProductId, row.WarehouseId), row);
  }

  return map;
};

const fetchRequisitionBacklog = async (
  warehouseIds: string[] | undefined,
  productIds: string[] | undefined
): Promise<Map<string, QuantityRow>> => {
  let query = `
    SELECT
      prl."ProductId" AS "ProductId",
      pr."WarehouseId" AS "WarehouseId",
      SUM(prl."Quantity" - prl."OrderedQuantity") AS "RequisitionBacklogQuantity"
    FROM "AppPurchaseRequisitionLines" prl
    INNER JOIN "AppPurchaseRequisitions" pr ON pr."Id" = prl."PurchaseRequisitionId"
    WHERE pr."IsDeleted" = false
      AND prl."Quantity" > prl."OrderedQuantity"
  `;
  const params: Array<string[]> = [];

  if (warehouseIds && warehouseIds.length > 0) {
    params.push(warehouseIds);
    query += ` AND pr."WarehouseId" = ANY($${params.length})`;
  }

  if (productIds && productIds.length > 0) {
    params.push(productIds);
    query += ` AND prl."ProductId" = ANY($${params.length})`;
  }

  query += ` GROUP BY prl."ProductId", pr."WarehouseId"`;

  const rows = await queryDb(query, params) as QuantityRow[];
  const map = new Map<string, QuantityRow>();

  for (const row of rows) {
    map.set(buildProductWarehouseKey(row.ProductId, row.WarehouseId), row);
  }

  return map;
};

const fetchSupplierOptions = async (
  productIds: string[],
  preferredSupplierOnly: boolean
): Promise<Map<string, SupplierOption[]>> => {
  if (productIds.length === 0) {
    return new Map<string, SupplierOption[]>();
  }

  let query = `
    SELECT
      sp."ProductId" AS "ProductId",
      sp."SupplierId" AS "SupplierId",
      s."Name" AS "SupplierName",
      spc."UnitId" AS "ConditionUnitId",
      unit."Name" AS "ConditionUnitName",
      spc."ConversionFactor" AS "ConversionFactor",
      sp."LeadTimeDays" AS "LeadTimeDays",
      sp."IsPreferred" AS "IsPreferred",
      spc."StandardPrice" AS "StandardPrice",
      spc."LastPurchasePrice" AS "LastPurchasePrice",
      spc."MinOrderQuantity" AS "MinOrderQuantity"
    FROM "AppSupplierProducts" sp
    INNER JOIN "AppSupplierProductConditions" spc ON spc."SupplierProductId" = sp."Id"
    INNER JOIN "AppSuppliers" s ON s."Id" = sp."SupplierId"
    INNER JOIN "AppBaseUnits" unit ON unit."Id" = spc."UnitId"
    WHERE sp."IsActive" = true
      AND s."IsDeleted" = false
      AND sp."ProductId" = ANY($1)
  `;
  const params: Array<string[]> = [productIds];

  if (preferredSupplierOnly) {
    query += ` AND sp."IsPreferred" = true`;
  }

  query += `
    ORDER BY
      sp."ProductId" ASC,
      sp."IsPreferred" DESC,
      sp."LeadTimeDays" ASC,
      spc."LastPurchasePrice" ASC,
      spc."StandardPrice" ASC,
      s."Name" ASC
  `;

  const rows = await queryDb(query, params) as SupplierOptionRow[];
  const map = new Map<string, SupplierOption[]>();

  for (const row of rows) {
    const productOptions = map.get(row.ProductId) || [];
    productOptions.push({
      supplierId: row.SupplierId,
      supplierName: row.SupplierName,
      conditionUnitId: row.ConditionUnitId,
      conditionUnitName: row.ConditionUnitName,
      conversionFactor: toNumber(row.ConversionFactor),
      leadTimeDays: toNumber(row.LeadTimeDays),
      isPreferred: row.IsPreferred,
      standardPrice: toNumber(row.StandardPrice),
      lastPurchasePrice: toNumber(row.LastPurchasePrice),
      minOrderQuantity: toNumber(row.MinOrderQuantity)
    });
    map.set(row.ProductId, productOptions);
  }

  return map;
};

const chooseBestSupplier = (options: SupplierOption[]): SupplierOption | null => {
  if (options.length === 0) {
    return null;
  }

  return options[0];
};

export const buildPurchasePlanAsync = async (input: PurchasePlanningInput): Promise<PurchasePlanningResult> => {
  const { historyStart, historyEnd, daysInMonth } = calculateForecastWindow(input.targetYear, input.targetMonth);
  const universe = await fetchProductWarehouseUniverse(input.warehouseIds, input.productIds, historyStart, historyEnd);

  const effectiveProductIds = input.productIds && input.productIds.length > 0
    ? input.productIds
    : Array.from(new Set(universe.map((item) => item.ProductId)));

  const [
    inventoryMap,
    forecastMap,
    inboundMap,
    backlogMap,
    supplierOptionsMap
  ] = await Promise.all([
    fetchInventorySnapshot(input.warehouseIds, input.productIds),
    fetchForecastDemand(input.warehouseIds, input.productIds, historyStart, historyEnd),
    fetchInboundPipeline(input.warehouseIds, input.productIds),
    input.includeRequisitionBacklog
      ? fetchRequisitionBacklog(input.warehouseIds, input.productIds)
      : Promise.resolve(new Map<string, QuantityRow>()),
    fetchSupplierOptions(effectiveProductIds, input.preferredSupplierOnly)
  ]);

  const items: PlanningLine[] = universe.map((item) => {
    const key = buildProductWarehouseKey(item.ProductId, item.WarehouseId);
    const inventory = inventoryMap.get(key);
    const forecast = forecastMap.get(key);
    const inbound = inboundMap.get(key);
    const backlog = backlogMap.get(key);
    const supplierOptions = supplierOptionsMap.get(item.ProductId) || [];
    const selectedSupplier = chooseBestSupplier(supplierOptions);

    const forecastDemand = toNumber(forecast?.ForecastDemand);
    const avgDailyDemand = daysInMonth > 0 ? forecastDemand / daysInMonth : 0;
    const safetyStockQuantity = avgDailyDemand * input.safetyStockDays;
    const onHandQuantity = toNumber(inventory?.OnHandQuantity);
    const lockedQuantity = toNumber(inventory?.LockedQuantity);
    const availableQuantity = toNumber(inventory?.AvailableQuantity);
    const incomingQuantity = toNumber(inbound?.IncomingQuantity);
    const requisitionBacklogQuantity = toNumber(backlog?.RequisitionBacklogQuantity);
    const rawNetRequiredQuantity = forecastDemand + safetyStockQuantity + requisitionBacklogQuantity - availableQuantity - incomingQuantity;
    const netRequiredQuantity = rawNetRequiredQuantity > 0 ? rawNetRequiredQuantity : 0;
    const minOrderQuantity = selectedSupplier ? selectedSupplier.minOrderQuantity : 0;
    const suggestedOrderQuantity = roundUpToIncrement(netRequiredQuantity, minOrderQuantity);
    const estimatedUnitPrice = selectedSupplier
      ? (selectedSupplier.lastPurchasePrice > 0 ? selectedSupplier.lastPurchasePrice : selectedSupplier.standardPrice)
      : null;
    const estimatedAmount = estimatedUnitPrice !== null ? estimatedUnitPrice * suggestedOrderQuantity : null;
    const warnings: string[] = [];

    if (forecastDemand === 0) {
      warnings.push("Sản phẩm không có lịch sử giao hàng trong 3 tháng gần nhất.");
    }

    if (!selectedSupplier) {
      warnings.push("Không tìm thấy nha cung cap active phu hop cho san pham.");
    }

    if (input.includeRequisitionBacklog && requisitionBacklogQuantity > 0) {
      warnings.push("Da cong backlog de nghi mua chua dat hang vao nhu cau.");
    }

    let reason = "Ton kha dung va hang dang ve da du.";
    if (suggestedOrderQuantity > 0) {
      reason = `Nhu cau du kien ${forecastDemand.toFixed(2)}, ton an toan ${safetyStockQuantity.toFixed(2)}, ton kha dung ${availableQuantity.toFixed(2)}, hang dang ve ${incomingQuantity.toFixed(2)}.`;
    }

    return {
      productId: item.ProductId,
      productCode: item.ProductCode,
      productName: item.ProductName,
      warehouseId: item.WarehouseId,
      warehouseCode: item.WarehouseCode,
      warehouseName: item.WarehouseName,
      forecastDemand,
      avgDailyDemand,
      safetyStockQuantity,
      onHandQuantity,
      lockedQuantity,
      availableQuantity,
      incomingQuantity,
      requisitionBacklogQuantity,
      netRequiredQuantity,
      suggestedOrderQuantity,
      recommendedSupplierId: selectedSupplier?.supplierId || null,
      recommendedSupplierName: selectedSupplier?.supplierName || null,
      recommendedUnitId: selectedSupplier?.conditionUnitId || null,
      recommendedUnitName: selectedSupplier?.conditionUnitName || null,
      conversionFactor: selectedSupplier?.conversionFactor || null,
      leadTimeDays: selectedSupplier?.leadTimeDays || null,
      estimatedUnitPrice,
      estimatedAmount,
      reason,
      warnings
    };
  });

  return {
    summary: {
      targetYear: input.targetYear,
      targetMonth: input.targetMonth,
      forecastMethod: input.forecastMethod,
      demandSource: input.demandSource,
      totalProductsAnalyzed: items.length,
      totalSuggestedLines: items.filter((item) => item.suggestedOrderQuantity > 0).length
    },
    items
  };
};
