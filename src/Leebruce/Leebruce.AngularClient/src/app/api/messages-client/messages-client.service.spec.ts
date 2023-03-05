import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthenticationModule } from '../authentication/authentication.module';
import { HttpErrorMapperModule } from '../http-error-mapper/http-error-mapper.module';
import { ServerErrorHandlingModule } from '../server-error-handling/server-error-handling.module';
import { MessagesClientModule } from './messages-client.module';
import { getFileSaver, FILESAVER } from './../fileSaver';

import { MessagesClientService } from './messages-client.service';

describe( 'MessagesClientService', () => {
  let service: MessagesClientService;
  let httpMock: HttpTestingController;

  beforeEach( () => {
    TestBed.configureTestingModule( {
      imports: [
        HttpClientTestingModule,
        ServerErrorHandlingModule,
        HttpErrorMapperModule,
        MessagesClientModule,
        AuthenticationModule
      ],
      providers: [
        { provide: FILESAVER, useFactory: getFileSaver }
      ]
    } );
    service = TestBed.inject( MessagesClientService );
    httpMock = TestBed.inject( HttpTestingController )
  } );

  it( 'should be created', () => {
    expect( service ).toBeTruthy();
  } );
} );
