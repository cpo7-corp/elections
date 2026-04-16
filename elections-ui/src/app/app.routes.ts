import { Routes } from '@angular/router';
import { ContactComponent } from './components/contact/contact.component';
import { SurveyComponent } from './components/survey/survey.component';

export const routes: Routes = [
  { path: 'survey', component: SurveyComponent },
  { path: 'survey/:id', component: SurveyComponent },
  { path: 'contact', component: ContactComponent },
  { path: '', redirectTo: 'survey', pathMatch: 'full' }
];
