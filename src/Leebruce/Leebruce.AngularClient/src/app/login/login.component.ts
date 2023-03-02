import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthenticationService, InvalidCredentialsError, loginDto } from '../api/authentication/authentication.service';
import { HttpValidationProblem } from '../api/problem-details';

@Component( {
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
} )
export class LoginComponent implements OnInit, OnDestroy {
  redirect$?: Subscription
  redirect: string | null = null;

  isInvalidCredentials = false
  previousCall$?: Subscription

  form: loginDto = {
    username: '',
    password: ''
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthenticationService ) {
  }

  ngOnInit(): void {
    this.redirect$ = this.route.queryParamMap.subscribe( params =>
      this.redirect = params.get( 'redirect' ) )
  }

  ngOnDestroy(): void {
    this.redirect$?.unsubscribe()
  }

  async redirectBack() {
    await this.router.navigate( [this.redirect ?? ''] );
  }

  onSubmit() {
    this.previousCall$?.unsubscribe()
    this.isInvalidCredentials = false

    this.authService.logIn( { password: this.form.password, username: this.form.username } ).subscribe( {
      complete: async () => await this.redirectBack(),
      error: error => {
        if ( error instanceof InvalidCredentialsError ) {
          this.isInvalidCredentials = true
        }
        if ( error instanceof HttpValidationProblem ) {
          //todo: temporary solution, show errors better
          this.isInvalidCredentials = true
        }

      }
    } )
  }

}
