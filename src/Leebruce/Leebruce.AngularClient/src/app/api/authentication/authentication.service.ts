import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';

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
        tap( resp => {
          this._token = resp
        } )
      );
  }

}

export interface loginDto {
  username: string;
  password: string;
}
