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

import { HttpError, HttpProblem, HttpValidationProblem, ProblemDetails, ValidationProblemDetails } from "../problem-details";

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  intercept( request: HttpRequest<unknown>, next: HttpHandler ): Observable<HttpEvent<unknown>> {
    return next.handle( request )
      .pipe( catchError( this.errorHandler ) );
  }

  errorHandler( response: HttpErrorResponse ) {
    if ( response.status === 0 ) {
      // A client-side or network error occurred. Handle it accordingly.
      return throwError( () => new HttpError( response, 'Something bad happened; server is unreachable; please try again later.' ) );
    }

    // The backend returned an unsuccessful response code.
    // The response body may contain clues as to what went wrong.

    if ( response.error?.error ) {
      // illformed body
      return throwError( () => new HttpError( response, 'Something bad happened; invalid server response; please try again later.' ) );
    }

    if ( ( response.error as ProblemDetails )?.status === undefined ) {
      // non-parsable error
      return throwError( () => new HttpError( response, response.error ) );
    }

    if ( ( response.error as ValidationProblemDetails )?.errors !== undefined ) {
      // validation problem details
      return throwError( () => new HttpValidationProblem( response, new ValidationProblemDetails( response.error ) ) );
    }

    if ( ( response.error as ProblemDetails )?.status !== undefined ) {
      // problem details
      return throwError( () => new HttpProblem( response, new ProblemDetails( response.error ) ) );
    }

    return throwError( () => new HttpError( response, 'Something bad happened; please try again later.' ) );
  }

}

export const ErrorInterceptorProvider = {
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true,
};
