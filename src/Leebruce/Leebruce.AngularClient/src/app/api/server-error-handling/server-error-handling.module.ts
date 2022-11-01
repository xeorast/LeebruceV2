import { NgModule } from '@angular/core';
import { ErrorNotifierModule } from '../error-notifier/error-notifier.module';

import { ServerErrorInterceptorProvider } from './server-error-handling.interceptor';



@NgModule( {
  declarations: [],
  imports: [
    ErrorNotifierModule
  ],
  providers: [
    ServerErrorInterceptorProvider
  ]
} )
export class ServerErrorHandlingModule { }
