import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { TimetableClientService, TimetableDayModel } from '../api/timetable-client/timetable-client.service';

import { TimetableComponent } from './timetable.component';
import { TimetableModule } from './timetable.module';

describe( 'TimetableComponent', () => {
  let component: TimetableComponent;
  let timetableService: jasmine.SpyObj<TimetableClientService>;
  let fixture: ComponentFixture<TimetableComponent>;
  let timetableSubject = new Subject<TimetableDayModel[]>();

  beforeEach( async () => {
    await TestBed.configureTestingModule( {
      imports: [TimetableModule],
      providers: [
        {
          provide: TimetableClientService,
          useValue: jasmine.createSpyObj( typeof TimetableClientService, ['getTimetable'] )
        }
      ]
    } )
      .compileComponents();

    timetableService = TestBed.inject( TimetableClientService ) as jasmine.SpyObj<TimetableClientService>
    timetableService.getTimetable.and.returnValue( timetableSubject.asObservable() )

    fixture = TestBed.createComponent( TimetableComponent );
    component = fixture.componentInstance;
    fixture.detectChanges();
  } );

  it( 'should create', () => {
    expect( component ).toBeTruthy();
  } );
} );
