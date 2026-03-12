import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors, HttpInterceptorFn } from '@angular/common/http';
import { provideClientHydration } from '@angular/platform-browser';

// Tarayıcının şifreli çerezleri API'ye (Cors üzerinden) yollamasına izin veren motor
const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const authReq = req.clone({
    withCredentials: true 
  });
  return next(authReq);
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideClientHydration(),
    provideHttpClient(
      withFetch(),
      withInterceptors([credentialsInterceptor])
    )
  ]
};