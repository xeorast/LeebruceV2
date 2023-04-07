import { Component, OnInit } from '@angular/core';
import { Modal } from 'bootstrap';
import { GradeModel, GradesClientService, GradesDataModel } from '../api/grades-client/grades-client.service';
import { GradesViewModel, SubjectGradesViewModel } from './grades.view-model';

@Component( {
  selector: 'app-grades',
  templateUrl: './grades.component.html',
  styleUrls: ['./grades.component.scss']
} )
export class GradesComponent implements OnInit {

  constructor(
    private gradesService: GradesClientService
  ) { }

  public model?: GradesViewModel
  public selected?: SubjectGradesViewModel
  public showOnlyNew: boolean = false

  public modalBody: string = ""
  private modalObj?: Modal

  ngOnInit(): void {
    let model: Partial<GradesViewModel> = {}
    let state = { fetchesRemaining: 2 }

    this.gradesService.getGrades().subscribe( res => {
      model.subjects = GradesComponent.mapResult( res )
      this.finishFetch( model, state )
    } )
    this.gradesService.getNewGrades().subscribe( res => {
      GradesComponent.mapNewToModel( res, model )
      this.finishFetch( model, state )
    } )

    let modalElem = document.getElementById( 'subjectGradesModal' )!
    this.modalObj = new Modal( modalElem, {} )
  }

  finishFetch( model: Partial<GradesViewModel>, state: { fetchesRemaining: number } ) {
    state.fetchesRemaining -= 1
    if ( state.fetchesRemaining == 0 ) {
      this.model = <GradesViewModel>model
    }
  }

  static mapResult( data: GradesDataModel ) {
    let subjectsVm = data.subjects as unknown as SubjectGradesViewModel[]
    for ( const subject of subjectsVm ) {
      if ( !data.isByPercent ) {
        let valuedGrades = subject.firstTermGrades.filter( g => g.value && g.value != 0 && g.weight && g.countToAverage )
          .concat( subject.secondTermGrades.filter( g => g.value && g.value != 0 && g.weight && g.countToAverage ) )

        subject.weightsSum = valuedGrades.reduce( ( sum: number, curr: GradeModel ) => sum + curr.weight!, 0 )
        subject.percent = subject.average ? ( subject.average - 1 ) * 100 / 5 : undefined
      }
    }

    return subjectsVm.filter( s => s.percent )
  }

  static mapNewToModel( data: GradesDataModel, model: Partial<GradesViewModel> ) {
    model.firstTermNewGrades = {}
    model.secondTermNewGrades = {}
    for ( const subject of data.subjects ) {
      model.firstTermNewGrades[subject.subject] = subject.firstTermGrades
      model.secondTermNewGrades[subject.subject] = subject.secondTermGrades
    }

  }

  select( subject: SubjectGradesViewModel ) {
    this.selected = subject
    this.modalObj?.show()

    this.modalBody = "" + subject.average
  }

  toggleShowOnlyNew() {
    this.showOnlyNew = !this.showOnlyNew
  }

  firstTermGrades() {
    if ( !this.selected )
      return undefined
    return this.showOnlyNew
      ? this.model?.firstTermNewGrades?.[this.selected.subject]
      : this.selected.firstTermGrades
  }
  secondTermGrades() {
    if ( !this.selected )
      return undefined
    return this.showOnlyNew
      ? this.model?.secondTermNewGrades?.[this.selected.subject]
      : this.selected.secondTermGrades
  }

}
