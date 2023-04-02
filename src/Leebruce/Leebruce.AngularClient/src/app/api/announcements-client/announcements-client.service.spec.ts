import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthenticationModule } from '../authentication/authentication.module';
import { HttpErrorMapperModule } from '../http-error-mapper/http-error-mapper.module';
import { ServerErrorHandlingModule } from '../server-error-handling/server-error-handling.module';
import { AnnouncementsClientModule } from './announcements-client.module';

import { AnnouncementsClientService } from './announcements-client.service';

describe( 'AnnouncementsClientService', () => {
  let service: AnnouncementsClientService;
  let httpMock: HttpTestingController;

  beforeEach( () => {
    TestBed.configureTestingModule( {
      imports: [
        HttpClientTestingModule,
        AuthenticationModule,
        ServerErrorHandlingModule,
        HttpErrorMapperModule,
        AnnouncementsClientModule,
      ]
    } );
    service = TestBed.inject( AnnouncementsClientService )
    httpMock = TestBed.inject( HttpTestingController )
  } );

  it( 'should be created', () => {
    expect( service ).toBeTruthy();
  } );
} );
