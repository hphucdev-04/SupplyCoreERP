import { Environment } from '@abp/ng.core';

const baseUrl = 'https://rxlogistics.vercel.app'; 
const backendUrl = 'https://rxlogistics.up.railway.app';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'RxLogistics',
  },
  oAuthConfig: {
    issuer: backendUrl + '/',
    redirectUri: baseUrl,
    clientId: 'SupplyCoreERP_App',
    responseType: 'code',
    scope: 'offline_access SupplyCoreERP',
    requireHttps: true,
  },
  apis: {
    default: {
      url: backendUrl,
      rootNamespace: 'SupplyCoreERP',
    },
    AbpAccountPublic: {
      url: backendUrl + '/',
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;