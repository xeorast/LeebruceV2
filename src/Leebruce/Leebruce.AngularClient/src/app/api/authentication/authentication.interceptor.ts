import {
  HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HTTP_INTERCEPTORS
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, NEVER, Observable } from 'rxjs';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';
import { AuthenticationService, IS_AUTH_ENABLED, IS_REDIRECT_TO_LOGIN, NotAuthenticatedError } from './authentication.service';

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {

  constructor(
    private router: Router,
    private authService: AuthenticationService ) { }

  intercept( request: HttpRequest<unknown>, next: HttpHandler ): Observable<HttpEvent<unknown>> {
    if ( request.context.get( IS_AUTH_ENABLED ) == true ) {
      request = request.clone( {
        headers: request.headers.set( 'Authorization', `Bearer ${this.authService.token}` )
      } );
    }

    let ret = next.handle( request )
    if ( request.context.get( IS_AUTH_ENABLED ) == true ) {
      ret = ret.pipe( catchError( error => this.authErrorMapper( error ) ) )
    }
    if ( request.context.get( IS_REDIRECT_TO_LOGIN ) == true ) {
      ret = ret.pipe( catchError( error => this.redirectToLoginErrorHandler( error ) ) )
    }

    return ret
  }

  private authErrorMapper( error: HttpError ): Observable<never> {
    let problemDetails: ProblemDetails | undefined
    if ( error instanceof HttpProblem ) {
      problemDetails = error.details
    }

    if ( error.response.status === 401 ) {
      this.authService.notifySessionEnded()
      throw new NotAuthenticatedError( problemDetails?.detail ?? undefined )
    }

    throw error
  }

  private redirectToLoginErrorHandler( error: HttpError ) {
    if ( error instanceof NotAuthenticatedError
      && !this.router.url.startsWith( '/login' ) ) {
      this.router.navigate( ['/login'], { queryParams: { redirect: this.router.url } } );
    }
    return NEVER
  }


}

export const AuthenticationInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: AuthenticationInterceptor,
  multi: true,
};
