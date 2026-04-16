import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrls: ['./app.less']
})
export class App {
  constructor(private translate: TranslateService) {
    const savedLang = localStorage.getItem('lang') || 'he';
    this.translate.setDefaultLang('he');
    this.useLanguage(savedLang);
  }

  useLanguage(lang: string) {
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
    document.documentElement.dir = lang === 'he' ? 'rtl' : 'ltr';
    document.documentElement.lang = lang;
  }

  toggleLanguage() {
    const newLang = this.translate.currentLang === 'he' ? 'en' : 'he';
    this.useLanguage(newLang);
  }
}
