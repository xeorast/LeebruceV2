import { Component } from '@angular/core';
import { Collapse } from 'bootstrap';

@Component( {
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
} )
export class NavMenuComponent {
  collapseNavMenu = true;
  collapse?: Collapse

  ngOnInit() {
    let modalElem = document.getElementById( 'navbarSupportedContent' )!
    this.collapse = new Collapse( modalElem, { toggle: false } )

  }

  toggleNavMenu() {
    // this.collapseNavMenu = !this.collapseNavMenu;
    this.collapse?.toggle()
  }
  hideNavMenu() {
    this.collapse?.hide()
  }
}
