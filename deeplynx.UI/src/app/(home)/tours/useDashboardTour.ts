// app/(home)/tours/useDashboardTour.ts
import { useEffect, useRef } from "react";
import Shepherd from "shepherd.js";
import type { Tour, Step, StepOptions, PopperPlacement } from "shepherd.js";
import { ProjectsList } from "../types/types";

interface UseDashboardTourProps {
  filteredProjects: ProjectsList[];
  initialProjects: ProjectsList[];
}

export function useDashboardTour({ 
  filteredProjects, 
  initialProjects 
}: UseDashboardTourProps) {
  const tourRef = useRef<Tour | null>(null);
  const builtRef = useRef(false);

  useEffect(() => {
    if (builtRef.current) return;
    builtRef.current = true;

    // Create tour instance
    const tour = new Shepherd.Tour({
      useModalOverlay: true,
      defaultStepOptions: {
        cancelIcon: { enabled: true },
        scrollTo: false,
        modalOverlayOpeningPadding: 8, // Increased padding around the highlighted element
        modalOverlayOpeningRadius: 8, // This creates rounded corners on the spotlight
      },
    });

    // Theme scoping class
    const addScope = () => document.body.classList.add("dlx-shepherd");
    const removeScope = () => document.body.classList.remove("dlx-shepherd");

    tour.on("start", addScope);
    tour.on("show", addScope);

    // Helper: wait until an element exists (or timeout)
    const waitForEl = (selector: string, timeout = 2000) =>
      new Promise<HTMLElement>((resolve, reject) => {
        const start = performance.now();
        const check = () => {
          const el = document.querySelector<HTMLElement>(selector);
          if (el) return resolve(el);
          if (performance.now() - start > timeout) {
            return reject(new Error(`Timeout waiting for ${selector}`));
          }
          requestAnimationFrame(check);
        };
        check();
      });

    // Build selectors for the first visible row
    const firstRowId = filteredProjects[0]?.id ?? 0;
    const toggleSel = `[data-tour="project-row-${firstRowId}-toggle"]`;
    const expandedSel = `[data-tour="project-row-${firstRowId}-expanded"]`;
    const closeSel = `[data-tour="project-row-${firstRowId}-close"]`;

    const closeExpandedRow = () => {
      const closeBtn = document.querySelector<HTMLElement>(closeSel);
      if (closeBtn && document.querySelector(expandedSel)) closeBtn.click();
    };

    // Define tour steps with proper types
    const steps: StepOptions[] = [
      {
        id: "intro",
        title: "Welcome to Your Dashboard! 👋",
        text: "Let's take a quick tour to help you get started with managing your projects and records.",
        buttons: [
          {
            text: "Skip",
            classes: "shepherd-button-secondary",
            action: () => tour.cancel(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      },
      {
        id: "search",
        title: "Search Your Projects",
        text: "Use this search bar to quickly find projects by name or description.",
        attachTo: { 
          element: "[data-tour='search-input']", 
          on: "bottom" as PopperPlacement 
        },
        scrollTo: false,
        classes: "shepherd-offset-bottom", 
        buttons: [
          {
            text: "Back",
            classes: "shepherd-button-secondary",
            action: () => tour.back(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      },
      {
        id: "projects-section",
        title: "Your Projects",
        text: "This is where all your projects are displayed. You can see project names, descriptions, and when they were last updated.",
        attachTo: { 
          element: "[data-tour='projects-section']", 
          on: "top" as PopperPlacement 
        },
        scrollTo: { behavior: "smooth" as ScrollBehavior, block: "center" as ScrollLogicalPosition },
        classes: "shepherd-offset-top",
        buttons: [
          {
            text: "Back",
            classes: "shepherd-button-secondary",
            action: () => tour.back(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      },
      {
        id: "project-row-toggle",
        title: "Expand a project",
        text: "Click this arrow to see more details about a project.",
        attachTo: { 
          element: toggleSel, 
          on: "left" as PopperPlacement 
        },
        scrollTo: { behavior: "smooth" as ScrollBehavior, block: "center" as ScrollLogicalPosition },
        classes: "shepherd-offset-left", 
        buttons: [
          {
            text: "Back",
            classes: "shepherd-button-secondary",
            action: () => tour.back(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      },
      {
        id: "add-record",
        title: "Add Records",
        text: "Click here to add new records to your existing projects. This helps you track progress and updates.",
        attachTo: { 
          element: "[data-tour='add-record']", 
          on: "bottom" as PopperPlacement 
        },
        scrollTo: false,
        classes: "shepherd-offset-bottom", 
        buttons: [
          {
            text: "Back",
            classes: "shepherd-button-secondary",
            action: () => tour.back(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      },
      {
        id: "create-project",
        title: "Create New Projects",
        text: "Start a new project by clicking this button. You can set up all the project details and begin tracking immediately.",
        attachTo: { 
          element: "[data-tour='create-project']", 
          on: "bottom" as PopperPlacement 
        },
        scrollTo: false,
        classes: "shepherd-offset-bottom",
        buttons: [
          {
            text: "Back",
            classes: "shepherd-button-secondary",
            action: () => tour.back(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      },
      {
        id: "complete",
        title: "You're All Set! 🎉",
        text: "That's it! You now know the basics. You can always restart this tour by clicking the help button in the header.",
        buttons: [
          {
            text: "Finish",
            action: () => {
              tour.complete();
              localStorage.setItem("dashboard-tour-completed", "true");
            },
          },
        ],
      },
    ];

    // Add the first 4 steps (intro, search, projects-section, project-row-toggle)
    steps.slice(0, 4).forEach(step => tour.addStep(step));

    // Add expanded project step if projects exist - this goes right after project-row-toggle
    if (filteredProjects.length > 0) {
      tour.addStep({
        id: "project-row-expanded",
        title: "Project details",
        text: "Here's the expanded panel with quick actions and recent info.",
        attachTo: { 
          element: expandedSel, 
          on: "top" as PopperPlacement 
        },
        scrollTo: { behavior: "smooth" as ScrollBehavior, block: "center" as ScrollLogicalPosition },
        // IMPORTANT: use function() { ... } to get the step via `this`
        beforeShowPromise: function () {
          const step = this as unknown as Step;
          return new Promise<void>(async (resolve) => {
            let expandedFound = !!document.querySelector(expandedSel);

            try {
              if (!expandedFound) {
                const btn = document.querySelector<HTMLElement>(toggleSel);
                btn?.click();

                // wait up to 3000ms for the expanded panel
                const waitForEl = (selector: string, timeout = 3000) =>
                  new Promise<HTMLElement>((res, rej) => {
                    const start = performance.now();
                    const tick = () => {
                      const el = document.querySelector<HTMLElement>(selector);
                      if (el) return res(el);
                      if (performance.now() - start > timeout)
                        return rej(new Error("timeout"));
                      requestAnimationFrame(tick);
                    };
                    tick();
                  });

                await waitForEl(expandedSel, 3000);
                expandedFound = true;
              }
            } catch {
              expandedFound = false;
            } finally {
              // If we still don't have the element, detach so Shepherd won't cancel
              if (!expandedFound) {
                step.updateStepOptions({ attachTo: undefined });
              }
              requestAnimationFrame(() => resolve());
            }
          });
        },
        when: { 
          hide: closeExpandedRow 
        },
        buttons: [
          {
            text: "Back",
            classes: "shepherd-button-secondary",
            action: () => tour.back(),
          },
          { text: "Next", action: () => tour.next() },
        ],
      });
    }

    // Add the remaining steps (add-record, create-project, complete)
    steps.slice(4).forEach(step => tour.addStep(step));

    // Event handlers for cleanup
    tour.on("cancel", () => {
      closeExpandedRow();
      removeScope();
    });
    
    tour.on("complete", () => {
      closeExpandedRow();
      removeScope();
    });

    // Store ref & maybe autostart
    tourRef.current = tour;

    const hasSeenTour = localStorage.getItem("dashboard-tour-completed");
    if (!hasSeenTour && initialProjects.length === 0) {
      setTimeout(() => tour.start(), 500);
    }

    // Cleanup
    return () => {
      try {
        tour.cancel();
      } catch {}
      document.body.classList.remove("dlx-shepherd");
    };
  }, [initialProjects.length]);

  const startTour = () => {
    if (tourRef.current) {
      tourRef.current.start();
    }
  };

  return { startTour };
}