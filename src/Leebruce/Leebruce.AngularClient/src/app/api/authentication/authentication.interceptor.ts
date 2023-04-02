import {
  HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HTTP_INTERCEPTORS
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';
import { AuthenticationService, IS_AUTH_ENABLED, NotAuthenticatedError } from './authentication.service';

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

    return next.handle( request )
      .pipe(
        catchError( error => this.authErrorMapper( error ) )
      )
  }

  private authErrorMapper( error: HttpError ): Observable<never> {
    let problemDetails: ProblemDetails | undefined
    if ( error instanceof HttpProblem ) {
      problemDetails = error.details
    }

    if ( error.response.status === 401 ) {
      throw new NotAuthenticatedError( problemDetails?.detail ?? undefined )
    }

    throw error
  }

}

export const AuthenticationInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: AuthenticationInterceptor,
  multi: true,
};
