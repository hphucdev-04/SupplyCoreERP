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
    name: '::Menu:DashBoard',
    iconClass: 'fas fa-chart-pie',
    order: 1,
    layout: eLayoutType.application,
  },

  ...createMenuGroup('Catalog', 'fas fa-map-marked-alt', 2, [
    { name: 'Category', icon: 'fas fa-globe' },
    { name: 'Medicine', icon: 'fas fa-flag' },
    { name: 'Medicine Unit', icon: 'fas fa-city' },
    { name: 'Active Ingredient', icon: 'fas fa-map-pin' },
    { name: 'Dosage Form', icon: 'fas fa-map-pin' },
  ], 'Catalog'),
];
