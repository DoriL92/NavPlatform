import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { EMPTY, throwError } from 'rxjs';
import { environment } from '../../environments/environment';

let isRedirecting = false;
const isAuthMe = (url: string) => {
  try { const u = new URL(url, window.location.origin); return u.pathname.endsWith('/auth/me'); }
  catch { return false; }
};

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const toGateway = req.url.startsWith(environment.gatewayBaseUrl);
  const withCreds = toGateway ? req.clone({ withCredentials: true }) : req;

  return next(withCreds).pipe(
    catchError((err: HttpErrorResponse) => {
      if (toGateway && req.method === 'GET' && isAuthMe(req.url) && err.status === 401 && !isRedirecting) {
        isRedirecting = true;
        const absoluteReturn = window.location.origin + window.location.pathname + window.location.search + window.location.hash;
        window.location.href = `${environment.gatewayBaseUrl}/auth/login?returnUrl=${encodeURIComponent(absoluteReturn)}`;
        return EMPTY;
      }
      return throwError(() => err);
    })
  );
};