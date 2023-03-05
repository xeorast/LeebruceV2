import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { MessageMetadataModel, MessagesClientService } from '../api/messages-client/messages-client.service';

import { MessagesComponent } from './messages.component';
import { MessagesModule } from './messages.module';

describe( 'MessagesComponent', () => {
  let component: MessagesComponent;
  let messagesService: jasmine.SpyObj<MessagesClientService>;
  let fixture: ComponentFixture<MessagesComponent>;
  let messagesSubject = new Subject<MessageMetadataModel[]>();

  beforeEach( async () => {
    await TestBed.configureTestingModule( {
      imports: [MessagesModule],
      providers: [
        {
          provide: MessagesClientService,
          useValue: jasmine.createSpyObj( typeof MessagesClientService, ['getMessages'] )
        }
      ]
    } )
      .compileComponents();

    messagesService = TestBed.inject( MessagesClientService ) as jasmine.SpyObj<MessagesClientService>
    messagesService.getMessages.and.returnValue( messagesSubject.asObservable() )

    fixture = TestBed.createComponent( MessagesComponent );
    component = fixture.componentInstance;
    fixture.detectChanges();
  } );

  it( 'should create', () => {
    expect( component ).toBeTruthy();
  } );
} );
