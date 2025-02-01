import React, { useState, useEffect, useCallback } from 'react';
import { Modal, Button, Text } from '@mantine/core';
import { useTimeout, useInterval } from '@mantine/hooks';

interface SessionTimeoutProps {
  children: React.ReactNode;
  onLogout: () => void;
}

const SessionTimeout = ({ children, onLogout }: SessionTimeoutProps) => {
  // Define durations (in milliseconds and seconds)
  const IDLE_TIMEOUT_MS = 15 * 60 * 1000; // 15 minutes inactivity
  const WARNING_COUNTDOWN_SECONDS = 60; // 60 seconds warning

  // State to manage the warning modal and countdown timer
  const [isWarningVisible, setWarningVisible] = useState(false);
  const [countdown, setCountdown] = useState(WARNING_COUNTDOWN_SECONDS);

  // useTimeout returns { start, clear }
  const { start: startIdleTimeout, clear: clearIdleTimeout } = useTimeout(() => {
    // When the timeout completes, show the warning modal
    setWarningVisible(true);
  }, IDLE_TIMEOUT_MS);

  // Reset the idle timer: clear then restart it
  const resetIdleTimeout = useCallback(() => {
    clearIdleTimeout();
    startIdleTimeout();
  }, [clearIdleTimeout, startIdleTimeout]);

  // Start the idle timer on mount and clean it up on unmount
  useEffect(() => {
    startIdleTimeout();
    return () => {
      clearIdleTimeout();
    };
  }, [startIdleTimeout, clearIdleTimeout]);

  // Reset the idle timer on user activity unless the warning modal is showing
  const resetTimerOnActivity = useCallback(() => {
    if (isWarningVisible) return;
    resetIdleTimeout();
  }, [isWarningVisible, resetIdleTimeout]);

  // Add event listeners for common user activity events
  useEffect(() => {
    window.addEventListener('mousemove', resetTimerOnActivity);
    window.addEventListener('keydown', resetTimerOnActivity);
    window.addEventListener('click', resetTimerOnActivity);
    return () => {
      window.removeEventListener('mousemove', resetTimerOnActivity);
      window.removeEventListener('keydown', resetTimerOnActivity);
      window.removeEventListener('click', resetTimerOnActivity);
    };
  }, [resetTimerOnActivity]);

// useInterval returns { start, stop, toggle, active }
const { start: startCountdown, stop: stopCountdown } = useInterval(() => {
    setCountdown((current) => {
      if (current <= 1) {
        stopCountdown();
        handleLogout();
        return 0;
      }
      return current - 1;
    });
  }, 1000);
  

  // Logout handler; uses clearCountdown to stop the countdown timer.
  const handleLogout = useCallback(() => {
    stopCountdown();
    onLogout();
  }, [stopCountdown, onLogout]);

  // When the warning modal is shown, start the countdown; otherwise, reset it.
useEffect(() => {
  if (isWarningVisible) {
    startCountdown();
  } else {
    stopCountdown();
    setCountdown(WARNING_COUNTDOWN_SECONDS);
  }
}, [isWarningVisible, startCountdown, stopCountdown]);

  // Handler for "Stay Logged In" action: hide the modal and reset the idle timer.
  const handleStayLoggedIn = () => {
    setWarningVisible(false);
    resetIdleTimeout();
  };

  return (
    <>
      {/* Warning Modal */}
      <Modal
        opened={isWarningVisible}
        onClose={handleStayLoggedIn}
        title="Session Timeout Warning"
        centered
      >
        <Text size="md" mb="md">
          You have been inactive for a while. You will be logged out in{' '}
          <strong>{countdown}</strong> seconds.
        </Text>
        <Button onClick={handleStayLoggedIn}>Stay Logged In</Button>
      </Modal>

      {/* Render the rest of your application */}
      {children}
    </>
  );
};

export default SessionTimeout;
