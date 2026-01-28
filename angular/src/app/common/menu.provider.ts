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
    { name: 'Categories', icon: 'fas fa-sitemap' },
    { name: 'Medicines', icon: 'fas fa-pills' },
    { name: 'Units', icon: 'fas fa-ruler-combined' },
    { name: 'Ingredients', icon: 'fas fa-flask' },
    { name: 'DosageForms', icon: 'fas fa-capsules' },
    { name: 'Manufacturers', icon: 'fas fa-industry' },
  ], 'Catalog'),

  ...createMenuGroup('Partner', 'fas fa-handshake', 3, [
    { name: 'Suppliers', icon: 'fas fa-truck' },
    { name: 'Customers', icon: 'fas fa-user-friends' },
  ], ),
];
