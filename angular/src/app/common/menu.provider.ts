import { eLayoutType } from '@abp/ng.core';

function createMenuGroup(
  name: string,
  icon: string,
  order: number,
  children?: {name: string; icon: string, requiredPolicy?: string}[],
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
    { name: 'Suppliers', icon: 'fas fa-truck' },
    { name: 'Customers', icon: 'fas fa-user-friends' },
  ]),

  ...createMenuGroup('Inventory', 'fas fa-warehouse', 4, [
    { name: 'Warehouses', icon: 'fas fa-building' },        // Quản lý Kho & Kệ
    { name: 'Batches', icon: 'fas fa-boxes' },              // Quản lý Lô & HSD
    { name: 'Tickets', icon: 'fas fa-file-invoice' },       // Phiếu Nhập/Xuất
    { name: 'Balances', icon: 'fas fa-clipboard-list' },    // Xem tồn kho
  ]),

  ...createMenuGroup('Order', 'fas fa-warehouse', 5, [
    { name: 'SaleOrders', icon: 'fas fa-building' },       
    { name: 'PurchaseOrders', icon: 'fas fa-boxes' },             

  ]),
];