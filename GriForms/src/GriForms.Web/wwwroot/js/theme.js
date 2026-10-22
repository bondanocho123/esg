// Tema terang / gelap / sistem. Dimuat sinkron di <head> agar kelas .dark sudah terpasang
// sebelum halaman digambar (tanpa kilatan tema terang).
// Pilihan disimpan di localStorage "gf-theme": "light" | "dark" | "system" (default).
(function () {
    const key = 'gf-theme';
    const media = window.matchMedia('(prefers-color-scheme: dark)');

    function preference() {
        try {
            return localStorage.getItem(key) || 'system';
        } catch {
            return 'system';
        }
    }

    function apply() {
        const pref = preference();
        const root = document.documentElement;
        // data-theme-pref dipakai tombol tema untuk menandai pilihan aktif
        root.dataset.themePref = pref;
        root.classList.toggle('dark', pref === 'dark' || (pref === 'system' && media.matches));
    }

    window.gfTheme = {
        apply,
        set(pref) {
            try {
                localStorage.setItem(key, pref);
            } catch { }
            apply();
        },
    };

    media.addEventListener('change', apply);
    apply();
})();
