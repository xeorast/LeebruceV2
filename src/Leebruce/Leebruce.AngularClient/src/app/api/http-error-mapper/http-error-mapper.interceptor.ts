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

import { ProblemDetails, ValidationProblemDetails } from "../problem-details";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  intercept( request: HttpRequest<unknown>, next: HttpHandler ): Observable<HttpEvent<unknown>> {
    return next.handle( request )
      .pipe( catchError( this.errorHandler ) );
  }

  errorHandler( error: HttpErrorResponse ) {
    if ( error.status === 0 ) {
      // A client-side or network error occurred. Handle it accordingly.
      return throwError( () => new Error( 'Something bad happened; server is unreachable; please try again later.' ) );
    }

    // The backend returned an unsuccessful response code.
    // The response body may contain clues as to what went wrong.

    if ( error.error?.error ) {
      // illformed body
      return throwError( () => new Error( 'Something bad happened; invalid server response; please try again later.' ) );
    }

    if ( ( error.error as ProblemDetails )?.status === undefined ) {
      // non-parsable error
      return throwError( () => new ProblemDetails( { status: error.status, detail: error.error } ) );
    }

    if ( ( error.error as ValidationProblemDetails )?.errors !== undefined ) {
      // validation problem details
      return throwError( () => new ValidationProblemDetails( error.error ) );
    }

    if ( ( error.error as ProblemDetails )?.status !== undefined ) {
      // problem details
      return throwError( () => new ProblemDetails( error.error ) );
    }

    return throwError( () => new Error( 'Something bad happened; please try again later.' ) );
  }

}

export const ErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true,
};
