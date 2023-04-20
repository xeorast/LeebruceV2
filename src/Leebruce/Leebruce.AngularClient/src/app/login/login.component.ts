import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthenticationService, InvalidCredentialsError, loginDto } from '../api/authentication/authentication.service';

@Component( {
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
} )
export class LoginComponent implements OnInit, OnDestroy {
  @ViewChild( 'passwordInput' )
  private passwordInput?: ElementRef

  form: loginDto = {
    username: '',
    password: ''
  };

  wasSubmited = false
  isProcessing = false
  isInvalidCredentials = false

  private redirect$?: Subscription
  private redirect: string | null = null;
  private previousCall$?: Subscription

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
    this.isProcessing = true

    this.previousCall$ = this.authService.logIn( { password: this.form.password, username: this.form.username } )
      .subscribe( {
        complete: async () => {
          this.isProcessing = false
          await this.redirectBack()
        },
        error: error => {
          this.isProcessing = false
          if ( error instanceof InvalidCredentialsError ) {
            this.isInvalidCredentials = true
            this.passwordInput?.nativeElement.setCustomValidity( "Invalid username or password." );
          }
        }
      } )
  }

}
