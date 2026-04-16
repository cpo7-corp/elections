import { Component, OnInit, HostListener, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-survey',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './survey.component.html',
  styleUrls: ['./survey.component.less']
})
export class SurveyComponent implements OnInit {
  private http = inject(HttpClient);
  private translate = inject(TranslateService);
  private cdr = inject(ChangeDetectorRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private baseApiUrl = environment.baseApiUrl;
  private userIdKey = 'elections_user_id';

  userId = '';
  parties: any[] = [];
  surveys: any[] = [];
  aggregatedResults: any[] = [];
  
  partyColors: string[] = [
    '#2563eb', '#f43f5e', '#10b981', '#f59e0b', '#8b5cf6', 
    '#06b6d4', '#ec4899', '#84cc16', '#6366f1', '#f97316',
    '#14b8a6', '#facc15', '#4ade80', '#60a5fa', '#fb7185',
    '#a78bfa', '#2dd4bf'
  ];
  
  page = 0;
  pageSize = 10;
  loading = false;
  isSubmitting = false;
  hasMore = true;

  ngOnInit() {
    this.initUser();
    this.loadParties();
    this.loadSurveys();
    this.loadAggregatedResults();
    
    this.route.params.subscribe(params => {
        if (params['id']) {
            this.loadSpecificSurvey(params['id']);
        }
    });
  }

  loadSpecificSurvey(id: string) {
      this.http.get<any>(`${this.baseApiUrl}/surveys/${id}`).subscribe(survey => {
          if (survey) {
              // We need to wait for parties to load
              const checkParties = setInterval(() => {
                  if (this.parties.length > 0) {
                      this.editSurvey(survey);
                      clearInterval(checkParties);
                  }
              }, 100);
          }
      });
  }

  initUser() {
    this.userId = localStorage.getItem(this.userIdKey) || '';
  }

  loadParties() {
    this.http.get<any[]>(`${this.baseApiUrl}/parties`).subscribe(data => {
      this.parties = data
        .sort((a, b) => (a.order || 0) - (b.order || 0))
        .map(p => ({ ...p, mandates: 0 }));
    });
  }

  loadAggregatedResults() {
    this.http.get<any[]>(`${this.baseApiUrl}/surveys/aggregated`).subscribe(data => {
      this.aggregatedResults = data;
    });
  }

  get totalMandates() {
    return this.parties.reduce((sum, p) => sum + (p.mandates || 0), 0);
  }

  submitSurvey() {
    if (this.totalMandates !== 120) {
      this.showAlert('ERROR_INVALID_TOTAL_MANDATES');
      return;
    }

    if (this.parties.some(p => p.mandates < 0 || p.mandates > 120)) {
        this.showAlert('ERROR_MANDATE_OUT_OF_RANGE');
        return;
    }

    const payload = {
      userId: this.userId || null,
      votes: this.parties.filter(p => p.mandates > 0).map(p => ({
        partyId: p.id,
        mandates: p.mandates
      }))
    };

    this.isSubmitting = true;
    this.http.post<any>(`${this.baseApiUrl}/surveys`, payload)
      .pipe(finalize(() => {
          this.isSubmitting = false;
          this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res) => {
          if (res.success) {
              if (res.data && res.data.userId) {
                  this.userId = res.data.userId;
                  localStorage.setItem(this.userIdKey, this.userId);
              }
              this.page = 0;
              this.surveys = [];
              this.hasMore = true;
              this.loadSurveys();
              this.loadAggregatedResults();
              this.showAlert('SUCCESS_SAVED');
          } else {
              this.showAlert(res.error || 'ERROR_SAVING');
          }
        },
        error: (err: any) => this.showAlert('ERROR_SAVING')
      });
  }

  private showAlert(key: string, params?: any) {
    this.translate.get(key, params).subscribe(msg => alert(msg));
  }

  loadSurveys() {
    if (this.loading || !this.hasMore) return;
    this.loading = true;
    this.http.get<any[]>(`${this.baseApiUrl}/surveys?page=${this.page}&pageSize=${this.pageSize}`)
      .pipe(finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
      }))
      .subscribe({
        next: (data) => {
          this.surveys = [...this.surveys, ...data];
          this.page++;
          if (data.length < this.pageSize) this.hasMore = false;
        },
        error: () => {
          this.hasMore = false;
        }
      });
  }

  like(survey: any) {
    this.http.post(`${this.baseApiUrl}/surveys/${survey.id}/like?userId=${this.userId}`, {})
      .subscribe(() => this.refreshSurvey(survey.id));
  }

  dislike(survey: any) {
    this.http.post(`${this.baseApiUrl}/surveys/${survey.id}/dislike?userId=${this.userId}`, {})
      .subscribe(() => this.refreshSurvey(survey.id));
  }

  private refreshSurvey(id: string) {
    this.page = 0;
    this.surveys = [];
    this.hasMore = true;
    this.loadSurveys();
    this.loadAggregatedResults();
  }

  @HostListener('window:scroll')
  onScroll() {
    if (this.loading || !this.hasMore) return;
    
    const pos = (document.documentElement.scrollTop || document.body.scrollTop) + document.documentElement.offsetHeight;
    const max = document.documentElement.scrollHeight;
    
    if (pos >= max - 100) {
      this.loadSurveys();
    }
  }

  canEdit(survey: any): boolean {
    if (survey.userId !== this.userId) return false;
    const created = new Date(survey.created).getTime();
    const now = new Date().getTime();
    return (now - created) < 15 * 60 * 1000;
  }

  editSurvey(survey: any) {
    this.parties.forEach(p => p.mandates = 0);
    survey.votes.forEach((v: any) => {
      const party = this.parties.find(p => p.id === v.partyId);
      if (party) party.mandates = v.mandates;
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  getPartyName(id: any) {
    return this.parties.find(p => p.id == id)?.name || id;
  }

  get partiesWithVotes() {
    return this.parties
      .filter(p => p.mandates > 0)
      .sort((a, b) => b.mandates - a.mandates);
  }

  getPieChartData() {
    let cumulativeMandates = 0;
    return this.partiesWithVotes.map((p, i) => {
      const start = (cumulativeMandates / 120) * 100;
      cumulativeMandates += p.mandates;
      const end = (cumulativeMandates / 120) * 100;
      return {
        name: p.name,
        color: this.partyColors[p.id % this.partyColors.length],
        percentage: (p.mandates / 120) * 100,
        start,
        end
      };
    });
  }

  getConicGradient() {
    let cumulative = 0;
    const parts = this.partiesWithVotes.map(p => {
      const color = this.partyColors[p.id % this.partyColors.length];
      const start = cumulative;
      cumulative += (p.mandates / 120) * 100;
      return `${color} ${start}% ${cumulative}%`;
    });
    return `conic-gradient(${parts.join(', ')}${parts.length > 0 ? ', #e2e8f0 ' + cumulative + '%' : '#e2e8f0 0%'})`;
  }

  getSurveyUrl(survey: any) {
      return `${window.location.origin}/survey/${survey.id}`;
  }

  copyLink(survey: any) {
      const url = this.getSurveyUrl(survey);
      navigator.clipboard.writeText(url).then(() => {
          this.showAlert('LINK_COPIED');
      });
  }

  openShare(survey: any) {
      this.router.navigate(['/survey', survey.id]);
      window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
