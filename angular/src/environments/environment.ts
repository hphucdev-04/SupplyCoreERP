import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44367/',
  redirectUri: baseUrl,
  clientId: 'SupplyCoreERP_App',
  responseType: 'code',
  scope: 'offline_access SupplyCoreERP',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'RxLogistics',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44367',
      rootNamespace: 'SupplyCoreERP',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
