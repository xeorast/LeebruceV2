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

  private updatesSubject = new ReplaySubject<StatusModel | "notLoggedIn">( 1 )
  public updatesSinceLastLogin = this.updatesSubject.asObservable()

  private getUpdatesSinceLastLogin() {
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<UpdatesModel>( 'api/Meta/updates-since-last-login', { context: context } )
  }

  private getUserName() {
    let context = AUTH_ENABLED_CONTEXT
    return this.http.get<string>( 'api/Meta/username', { context: context, headers: { accept: 'application/json' } } )
  }

  private fetchAndPushUpdates() {
    let model = <StatusModel>{}

    this.getUpdatesSinceLastLogin().subscribe( {
      next: updates => {
        model.updates = updates
        this.pushIfComplete( model )
      }
    } )

    this.getUserName().subscribe( {
      next: userName => {
        model.userName = userName
        this.pushIfComplete( model )
      }
    } )
  }

  private pushIfComplete( model: StatusModel ) {
    if ( model.updates != null && model.userName != null ) {
      this.updatesSubject.next( model )
    }
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

export interface StatusModel {
  userName: string
  updates: UpdatesModel
}

export interface UpdatesModel {
  newGrades: number
  newAbsences: number
  newMessages: number
  newAnnouncements: number
  newEvents: number
  newHomeworks: number
}
