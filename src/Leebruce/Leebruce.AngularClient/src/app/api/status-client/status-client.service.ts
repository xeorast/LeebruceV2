import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { AuthenticationService, AUTH_ENABLED_CONTEXT, loginStatus } from '../authentication/authentication.service';

@Injectable()
export class StatusClientService {

  constructor(
    private http: HttpClient,
    authService: AuthenticationService ) {
    authService.loginStatus.subscribe( { next: status => this.onLoginStatusChange( status ) } )
  }

  private updatesSubject = new ReplaySubject<UpdatesModel | "notLoggedIn">( 1 )
  public updatesSinceLastLogin = this.updatesSubject.asObservable()

  private getUpdatesSinceLastLogin() {
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<UpdatesModel>( 'api/Meta/updates-since-last-login', { context: context } )
  }

  private fetchAndPushUpdates() {
    let fetch$ = this.getUpdatesSinceLastLogin().subscribe( {
      next: updates => {
        this.updatesSubject.next( updates )
        fetch$.unsubscribe()
      }
    } )
  }

  private onLoginStatusChange( status: loginStatus ) {
    if ( status.status == "authenticated" ) {
      this.fetchAndPushUpdates()
    }
    else {
      this.updatesSubject.next( "notLoggedIn" )
    }
  }

}

export interface UpdatesModel {
  newGrades: number
  newAbsences: number
  newMessages: number
  newAnnouncements: number
  newEvents: number
  newHomeworks: number
}
