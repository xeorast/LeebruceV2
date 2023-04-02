import { NgModule } from '@angular/core';

import { AuthenticationInterceptorProvider } from './authentication.interceptor';
import { AuthenticationService } from './authentication.service';



@NgModule( {
  declarations: [],
  imports: [],
  providers: [
    AuthenticationService,
    AuthenticationInterceptorProvider,
  ]
} )
export class AuthenticationModule { }
