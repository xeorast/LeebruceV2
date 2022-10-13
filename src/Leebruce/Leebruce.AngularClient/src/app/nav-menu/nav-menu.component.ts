import { Component } from '@angular/core';

@Component( {
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
} )
export class NavMenuComponent {
  collapseNavMenu = true;

  toggleNavMenu() {
    this.collapseNavMenu = !this.collapseNavMenu;
  }
}
