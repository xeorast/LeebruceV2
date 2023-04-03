import { Component } from '@angular/core';
import { Collapse } from 'bootstrap';
import { StatusClientService, UpdatesModel } from '../api/status-client/status-client.service';

@Component( {
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
} )
export class NavMenuComponent {
  collapseNavMenu = true;
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

  onStatusChanged( updates: UpdatesModel | "notLoggedIn" ) {
    this.updates = updates == "notLoggedIn" ? null : updates
    this.hasUpdates = updates != "notLoggedIn"
      && ( updates.newAbsences
        || updates.newAnnouncements
        || updates.newEvents
        || updates.newGrades
        || updates.newHomeworks
        || updates.newMessages ) > 0
  }

  toggleNavMenu() {
    // this.collapseNavMenu = !this.collapseNavMenu;
    this.collapse?.toggle()
  }
  hideNavMenu() {
    this.collapse?.hide()
  }
}
