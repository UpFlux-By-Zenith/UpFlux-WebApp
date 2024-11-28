import { Container, Card, Image, Text, Title} from '@mantine/core';
import './about.css';
import updateimg from "../../assets/images/update.jpg";
import rollbackimg from "../../assets/images/rollback.jpg";
import monitoringimg from "../../assets/images/monitoring.jpg";
import schedulingimg from "../../assets/images/scheduling.jpg";

export const About = () => {
  return (
    <section id="about" className="about section light-background">
      <Container size="lg" className="container">
        <div className="about-content">
          <div className="about-text">
            <Title order={3}>About Us</Title>
            <Title order={2} style={{ marginBottom: '1rem' }}>
              Streamlining Software Updates in Paper Mills with UpFlux
            </Title>
            <Text size="lg">
              UpFlux was created to revolutionize how updates are managed in paper mills. Our platform minimizes downtime by efficiently handling updates, rollbacks, and version control, all while maintaining peak operational performance. With intelligent scheduling powered by AI and real-time monitoring, UpFlux is dedicated to enhancing productivity and simplifying the update process.
            </Text>
          </div>

          <div className="icon-boxes">
            <Card className="icon-box">
              <Card.Section>
                <Image src={updateimg} alt="Automated Update Management" className="icon-image" />
              </Card.Section>
              <Title order={4} className="icon-title">Automated Update Management</Title>
              <Text>
                Streamlines software updates across multiple production machines, reducing downtime and manual effort.
              </Text>
            </Card>

            <Card className="icon-box">
              <Card.Section>
                <Image src={rollbackimg} alt="Version Control & Rollback" className="icon-image" />
              </Card.Section>
              <Title order={4} className="icon-title">Version Control & Rollback</Title>
              <Text>
                Tracks update history and supports quick rollbacks to previous versions if issues arise.
              </Text>
            </Card>

            <Card className="icon-box">
              <Card.Section>
                <Image src={monitoringimg} alt="Real-Time Monitoring" className="icon-image" />
              </Card.Section>
              <Title order={4} className="icon-title">Real-Time Monitoring</Title>
              <Text>
                Provides live status updates and health checks for machines to ensure optimal performance.
              </Text>
            </Card>

            <Card className="icon-box">
              <Card.Section>
                <Image src={schedulingimg} alt="Intelligent Scheduling" className="icon-image" />
              </Card.Section>
              <Title order={4} className="icon-title">Intelligent Scheduling</Title>
              <Text>
                Uses AI to schedule updates at the most efficient times, minimizing impact on production.
              </Text>
            </Card>
          </div>
        </div>
      </Container>
    </section>
  );
};
