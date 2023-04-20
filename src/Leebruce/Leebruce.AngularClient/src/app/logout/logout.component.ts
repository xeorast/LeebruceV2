import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../api/authentication/authentication.service';

@Component( {
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: []
} )
export class LogoutComponent implements OnInit {

  constructor(
    private router: Router,
    private authService: AuthenticationService ) { }

  ngOnInit(): void {
    this.authService.logOut().subscribe( {
      next: () => {
        this.router.navigate( ['/'] );
      }
    } )
  }

}
