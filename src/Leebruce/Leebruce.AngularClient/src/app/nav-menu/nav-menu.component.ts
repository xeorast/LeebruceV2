import { Component } from '@angular/core';
import { Collapse } from 'bootstrap';
import { StatusClientService, StatusModel, UpdatesModel } from '../api/status-client/status-client.service';
import { AuthenticationService, loginStatus } from '../api/authentication/authentication.service';

@Component( {
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
} )
export class NavMenuComponent {
  collapse?: Collapse
  updates: UpdatesModel | null = null
  hasUpdates: boolean = false
  isLoggedIn = false

  constructor(
    authService: AuthenticationService,
    statusService: StatusClientService ) {
    statusService.updatesSinceLastLogin.subscribe( { next: updates => this.onStatusChanged( updates ) } )
    authService.loginStatus.subscribe( { next: status => this.onLoginStatusChange( status ) } )
  }

  ngOnInit() {
    let modalElem = document.getElementById( 'navbarSupportedContent' )!
    this.collapse = new Collapse( modalElem, { toggle: false } )

  }

  onStatusChanged( status: StatusModel | "notLoggedIn" ) {
    this.updates = status == "notLoggedIn" ? null : status.updates
    this.hasUpdates = status != "notLoggedIn"
      && ( status.updates.newAbsences
        || status.updates.newAnnouncements
        || status.updates.newEvents
        || status.updates.newGrades
        || status.updates.newHomeworks
        || status.updates.newMessages ) > 0
  }

  private onLoginStatusChange( status: loginStatus ) {
    this.isLoggedIn = status.status == "authenticated"
  }

  toggleNavMenu() {
    this.collapse?.toggle()
  }
  hideNavMenu() {
    this.collapse?.hide()
  }
}
