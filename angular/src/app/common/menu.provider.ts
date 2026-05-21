import { eLayoutType } from '@abp/ng.core';

function createMenuGroup(
  name: string,
  icon: string,
  order: number,
  children?: { name: string; icon: string, requiredPolicy?: string }[],
  policy?: string,
) {
  const parentName = `::Menu:${name}`;
  const parentPath = `/${name.toLowerCase()}`;

  const parent = {
    path: parentPath,
    name: parentName,
    iconClass: icon,
    order,
    layout: eLayoutType.application,
    requiredPolicy: policy,
  };

  if (!children || children.length === 0) {
    return [parent];
  }

  return [
    parent,
    ...children.map((c, i) => ({
      path: `${parentPath}/${c.name.toLowerCase()}`,
      name: `::Menu:${c.name}`,
      parentName,
      iconClass: c.icon,
      order: i + 1,
      layout: eLayoutType.application,
      requiredPolicy: c.requiredPolicy || policy,
    })),
  ];
}

export const APP_ROUTES = [
  {
    path: '/',
    name: '::Menu:Dashboard',
    iconClass: 'fas fa-chart-line',
    order: 1,
    layout: eLayoutType.application,
  },

  ...createMenuGroup('Catalog', 'fas fa-layer-group', 2, [
    { name: 'Categories', icon: 'fas fa-sitemap', requiredPolicy: 'Catalog.Category' },
    { name: 'Medicines', icon: 'fas fa-pills', requiredPolicy: 'Catalog.Medicine' },
    { name: 'Units', icon: 'fas fa-ruler-combined', requiredPolicy: 'Catalog.BaseUnit' },
    { name: 'Ingredients', icon: 'fas fa-flask', requiredPolicy: 'Catalog.ActiveIngredient' },
    { name: 'DosageForms', icon: 'fas fa-capsules', requiredPolicy: 'Catalog.DosageForm' },
    { name: 'Manufacturers', icon: 'fas fa-industry', requiredPolicy: 'Catalog.Manufacturer' },
  ]),

  ...createMenuGroup('Partner', 'fas fa-handshake', 3, [
    { name: 'Suppliers', icon: 'fas fa-truck' , requiredPolicy: 'Partner.Supplier' },
    { name: 'Customers', icon: 'fas fa-user-friends', requiredPolicy: 'Partner.Customer' },
  ]),
  ...createMenuGroup('Order', 'fas fa-clipboard-list', 4, [
    { name: 'PurchaseRequisitions', icon: 'fas fa-file-medical' , requiredPolicy: 'Order.PurchaseRequisition'},
    { name: 'PurchaseOrders', icon: 'fas fa-file-invoice-dollar' , requiredPolicy: 'Order.PurchaseOrder'},
    { name: 'SaleOrders', icon: 'fas fa-shipping-fast' , requiredPolicy: 'Order.SaleOrder'},
  ]),
  ...createMenuGroup('Inventory', 'fas fa-warehouse', 5, [
    { name: 'Warehouses', icon: 'fas fa-building' , requiredPolicy: 'Inventory.Warehouse'},        
    { name: 'Batches', icon: 'fas fa-boxes' , requiredPolicy: 'Inventory.Batch'},              
    { name: 'Tickets', icon: 'fas fa-file-invoice' , requiredPolicy: 'Inventory.Ticket'},        
    { name: 'Balances', icon: 'fas fa-clipboard-list' ,},   
  ]),

];