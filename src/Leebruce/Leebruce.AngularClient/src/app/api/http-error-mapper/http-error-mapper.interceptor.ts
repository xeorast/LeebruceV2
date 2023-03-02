import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HTTP_INTERCEPTORS
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { ErrorNotifierService, ServerConnectionError, ServerError } from '../error-notifier/error-notifier.service';

import { HttpError, HttpProblem, HttpValidationProblem, ProblemDetails, ValidationProblemDetails } from "../problem-details";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor( private errorService: ErrorNotifierService ) {
  }

  intercept( request: HttpRequest<unknown>, next: HttpHandler ): Observable<HttpEvent<unknown>> {
    return next.handle( request )
      .pipe( catchError( this.errorHandler ) );
  }

  errorHandler( response: HttpErrorResponse ) {
    let error: HttpError | undefined

    if ( response.status === 0 ) {
      // A client-side or network error occurred. Handle it accordingly.
      error = new ServerConnectionError( response, 'Server is unreachable; please try again later' )
    }

    // The backend returned an unsuccessful response code.
    // The response body may contain clues as to what went wrong.

    else if ( response.error?.error ) {
      // illformed body
      error = new ServerError( response, 'Something bad happened; server returned invalid response; please try again later.' )
    }

    else if ( ( response.error as ProblemDetails )?.status === undefined ) {
      // non-parsable error
      error = new HttpError( response, response.error )
    }

    else if ( ( response.error as ValidationProblemDetails )?.errors !== undefined ) {
      // validation problem details
      error = new HttpValidationProblem( response, new ValidationProblemDetails( response.error ) )
    }

    else if ( ( response.error as ProblemDetails )?.status !== undefined ) {
      // problem details
      error = new HttpProblem( response, new ProblemDetails( response.error ) )
    }

    if ( error instanceof ServerError ) {
      this.errorService.raiseError( error )
    }

    return throwError( () => error ?? new ServerError( response, 'Something bad happened; please try again later.' ) );
  }

}

export const ErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true,
};
