import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HTTP_INTERCEPTORS
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { ErrorNotifierService, ProxyError, ProxyTimeoutError, ServerError, TechnicalBreakError } from '../error-notifier/error-notifier.service';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable()
export class ServerErrorHandlingInterceptor implements HttpInterceptor {

  constructor( private errorService: ErrorNotifierService ) {
  }

  intercept( request: HttpRequest<unknown>, next: HttpHandler ): Observable<HttpEvent<unknown>> {
    return next.handle( request )
      .pipe( catchError( this.errorHandler ) );;
  }

  errorHandler( error: HttpError ) {
    let problemDetails: ProblemDetails | undefined
    if ( error instanceof HttpProblem ) {
      problemDetails = error.details
    }

    let serverError: ServerError | undefined;
    if ( error.response.status === 500 ) {
      serverError = new ServerError( error.response, problemDetails?.detail ?? 'Server error occureed, try again later' )
    }

    if ( error.response.status === 502 ) {
      serverError = new ProxyError( error.response, problemDetails?.detail ?? 'Proxied server error occured, try again later' )
    }

    if ( error.response.status === 503 ) {
      serverError = new TechnicalBreakError( error.response, problemDetails?.detail ?? 'Proxied server is down for maintenance, try again later' )
    }

    if ( error.response.status === 504 ) {
      serverError = new ProxyTimeoutError( error.response, problemDetails?.detail ?? 'Proxied server did not respond, try again later' )
    }

    if ( serverError !== undefined ) {
      this.errorService.raiseError( serverError )
      return throwError( () => serverError )
    }

    throw error;
  }

}

export const ServerErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ServerErrorHandlingInterceptor,
  multi: true,
};
