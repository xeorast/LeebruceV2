import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthenticationService, IS_AUTH_ENABLED } from './authentication.service';

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

  constructor(
    private authService: AuthenticationService ) { }

  intercept( request: HttpRequest<unknown>, next: HttpHandler ): Observable<HttpEvent<unknown>> {
    if ( request.context.get( IS_AUTH_ENABLED ) == true ) {
      request = request.clone( {
        headers: request.headers.set( 'Authorization', `Bearer ${this.authService.token}` )
      } );
    }

    return next.handle( request );
  }
}

export const AuthenticationInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: AuthenticationInterceptor,
  multi: true,
};
