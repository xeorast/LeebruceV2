import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthenticationModule } from '../authentication/authentication.module';
import { HttpErrorMapperModule } from '../http-error-mapper/http-error-mapper.module';
import { ServerErrorHandlingModule } from '../server-error-handling/server-error-handling.module';
import { TimetableClientModule } from './timetable-client.module';

import { TimetableClientService } from './timetable-client.service';

describe( 'TimetableClientService', () => {
  let service: TimetableClientService;

  beforeEach( () => {
    TestBed.configureTestingModule( {
      imports: [
        HttpClientTestingModule,
        AuthenticationModule,
        ServerErrorHandlingModule,
        HttpErrorMapperModule,
        TimetableClientModule,
      ]
    } );
    service = TestBed.inject( TimetableClientService );
  } );

  it( 'should be created', () => {
    expect( service ).toBeTruthy();
  } );
} );
