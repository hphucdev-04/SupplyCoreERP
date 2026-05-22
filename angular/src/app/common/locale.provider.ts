import { registerLocaleData } from '@angular/common';
import localeEn from '@angular/common/locales/en';
import localeVi from '@angular/common/locales/vi';

export function registerLocales() {
  return () => {
    return Promise.resolve().then(() => {
      registerLocaleData(localeEn, 'en');
      registerLocaleData(localeVi, 'vi');
    });
  };
}
