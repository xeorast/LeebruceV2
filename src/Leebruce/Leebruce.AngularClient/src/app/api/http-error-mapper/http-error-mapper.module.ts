import { NgModule } from '@angular/core';

import { ErrorInterceptorProvider } from './http-error-mapper.interceptor';



@NgModule( {
  declarations: [],
  imports: [],
  providers: [
    ErrorInterceptorProvider
  ]
} )
export class HttpErrorMapperModule { }
