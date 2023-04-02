import { HttpClient, HttpContext, HttpContextToken } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, tap, throwError } from 'rxjs';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

export const IS_AUTH_ENABLED = new HttpContextToken( () => false );
export const AUTH_ENABLED_CONTEXT = new HttpContext().set( IS_AUTH_ENABLED, true )

export const IS_REDIRECT_TO_LOGIN = new HttpContextToken( () => false );
export const AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT = new HttpContext().set( IS_REDIRECT_TO_LOGIN, true )

@Injectable()
export class AuthenticationService {

  constructor( private http: HttpClient ) {
    this._token = this.retrieveToken()
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
    return this.http.post( 'api/auth/login', credentials, { responseType: 'text' } )
      .pipe(
        tap( token => this.setToken( token ) ),
        map( _token => { } ),
        catchError( this.errorHandler )
      );
  }

  private setToken( token: string ) {
    this._token = token
    this.storeToken( token )
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

  private storeToken( token: string ) {
    sessionStorage.setItem( 'access-token', token )
  }

  private retrieveToken() {
    return sessionStorage.getItem( 'access-token' ) ?? undefined
  }

}

export interface loginDto {
  username: string;
  password: string;
}

export class InvalidCredentialsError extends Error {
}

export class NotAuthenticatedError extends Error {
}