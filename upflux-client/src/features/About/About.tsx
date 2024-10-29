import './about.css';

//Image from https://depositphotos.com/46719109/stock-photo-update-icon.html
import updateimg from "../../assets/images/update.jpg";
//Image from https://rmhbc.ca/ways-to-give/
import rollbackimg from "../../assets/images/rollback.jpg";
//Image from https://www.vectorstock.com/royalty-free-vector/charts-monitoring-rounded-icon-vector-10489125
import monitoringimg from "../../assets/images/monitoring.jpg";
//Image from https://pixabay.com/en/calendar-dates-schedule-date-2027122/
import schedulingimg from "../../assets/images/scheduling.jpg";

const About = () => {
  return (
    <section id="about" className="about section light-background">
      <div className="container">
        <div className="about-content">
          <div className="about-text">
            <h3>About Us</h3>
            <h2>Streamlining Software Updates in Paper Mills with UpFlux</h2>
            <p>
            UpFlux was created to revolutionize how updates are managed in paper mills.
            Our platform minimizes downtime by efficiently handling updates, rollbacks, 
            and version control, all while maintaining peak operational performance. 
            With intelligent scheduling powered by AI and real-time monitoring, 
            UpFlux is dedicated to enhancing productivity and simplifying the update process.
            </p>
            <a href="#" className="read-more">
              <i className="bi bi-arrow-right"></i>
            </a>
          </div>

          <div className="icon-boxes">
            <div className="icon-box">
            <img src={updateimg} alt="Icon 1" className="icon-image" />
              <h3>Automated Update Management</h3>
              <p>Streamlines software updates across multiple production machines, reducing downtime and manual effort.</p>
            </div>

            <div className="icon-box">
            <img src={rollbackimg} alt="Icon 1" className="icon-image" />
              <h3>Version Control & Rollback</h3>
              <p>Tracks update history and supports quick rollbacks to previous versions if issues arise.</p>
            </div>

            <div className="icon-box">
            <img src={monitoringimg} alt="Icon 1" className="icon-image" />
              <h3>Real-Time Monitoring</h3>
              <p>Provides live status updates and health checks for machines to ensure optimal performance.</p>
            </div>

            <div className="icon-box">
            <img src={schedulingimg} alt="Icon 1" className="icon-image" />
              <h3>Intelligent Scheduling</h3>
              <p>Uses AI to schedule updates at the most efficient times, minimizing impact on production.</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default About;
