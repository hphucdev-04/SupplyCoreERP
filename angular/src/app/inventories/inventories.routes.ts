import { Routes } from '@angular/router';

export const INVENTORIES_ROUTES: Routes = [
    {

        path: 'warehouses',
        loadComponent: () => import('./warehouses/warehouses.component').then(m => m.WarehousesComponent),
    },
   {
        path: 'warehouses/layouts/:id',
        loadComponent: () => import('./warehouses/storage-locations/storage-locations.component').then(m => m.StorageLocationsComponent),
    },
    {
        path: 'tickets',
        loadComponent: () => import('./tickets/tickets.component').then(m => m.TicketsComponent),
    },
    {
        path: 'tickets/details/:id',
        loadComponent: () => import('./tickets/tickets-details/ticket-details.component').then(m => m.TicketDetailsComponent),
    },
    {
        path: 'batches',
        loadComponent: () => import('./batches/batches.component').then(m => m.BatchesComponent),
    },
    {
        path: 'balances',
        loadComponent: () => import('./balances/balances.component').then(m => m.BalancesComponent),
    },

];