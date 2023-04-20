import { Component } from '@angular/core';
import { Collapse } from 'bootstrap';
import { StatusClientService, StatusModel, UpdatesModel } from '../api/status-client/status-client.service';

@Component( {
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
} )
export class NavMenuComponent {
  collapse?: Collapse
  updates: UpdatesModel | null = null
  hasUpdates: boolean = false

  constructor(
    statusService: StatusClientService ) {
    statusService.updatesSinceLastLogin.subscribe( { next: updates => this.onStatusChanged( updates ) } )
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

  toggleNavMenu() {
    this.collapse?.toggle()
  }
  hideNavMenu() {
    this.collapse?.hide()
  }
}
