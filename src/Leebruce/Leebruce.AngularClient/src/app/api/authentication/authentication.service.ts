import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, tap, throwError } from 'rxjs';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable()
export class AuthenticationService {

  constructor( private http: HttpClient ) {
  }

  private _token?: string;
  public get token(): string | null {
    return this._token ?? null;
  }
  public get isLoggedIn(): boolean {
    return this._token !== undefined
      && this._token !== null;
  }

  public logIn( credentials: loginDto ) {
    return this.http.post<string>( 'api/auth/login', credentials )
      .pipe(
        tap( token => this.setToken( token ) ),
        map( _token => { } ),
        catchError( this.errorHandler )
      );
  }

  private setToken( token: string ) {
    this._token = token
  }

  private errorHandler( error: HttpError ) {
    let problemDetails: ProblemDetails | undefined
    if ( error instanceof HttpProblem ) {
      problemDetails = error.details
    }

    if ( error.response.status === 401 ) {
      return throwError( () => new InvalidCredentialsError( problemDetails?.detail ?? undefined ) )
    }

    throw error
  }

}

export interface loginDto {
  username: string;
  password: string;
}

export class InvalidCredentialsError extends Error {
}