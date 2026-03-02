import { Routes } from '@angular/router';

export const INVENTORIES_ROUTES: Routes = [
    {

        path: 'warehouses',
        loadComponent: () => import('./warehouses/warehouses.component').then(m => m.WarehousesComponent),
    },
   {
        // ✅ Load đúng StorageLocationsComponent
        path: 'warehouses/:id/locations',
        loadComponent: () => import('./warehouses/storage-locations/storage-locations.component').then(m => m.StorageLocationsComponent),
    },
    // {
    //     // Path thực tế sẽ là: /warehouse/batches
    //     path: 'batches',
    //     loadComponent: () => import('./batches/batches.component').then(m => m.BatchesComponent),
    // },
    // {
    //     // Path thực tế sẽ là: /warehouse/tickets
    //     path: 'tickets',
    //     loadComponent: () => import('./tickets/tickets.component').then(m => m.TicketsComponent),
    // },
    // {
    //     // Path thực tế sẽ là: /warehouse/balances
    //     path: 'balances',
    //     loadComponent: () => import('./balances/balances.component').then(m => m.BalancesComponent),
    // }
];