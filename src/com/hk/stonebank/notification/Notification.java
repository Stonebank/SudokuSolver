package com.hk.stonebank.notification;

import java.awt.*;
import java.util.Timer;
import java.util.TimerTask;

public class Notification {

    private final TrayIcon trayIcon;

    private final String message;

    public Notification(String message) {
        this.trayIcon = new TrayIcon(Toolkit.getDefaultToolkit().getImage(""), "SudokuSolver");
        this.message = message;
        if (!SystemTray.isSupported()) {
            System.err.println("Notification is not supported on this machine.");
            return;
        }
        try {
            SystemTray systemTray = SystemTray.getSystemTray();
            systemTray.add(trayIcon);
        } catch (AWTException e) {
            System.err.println("Tray icon was not added to the system tray.");
            e.printStackTrace();
        }
    }

    private void close() {
        new Timer().schedule(new TimerTask() {
            @Override
            public void run() {
                SystemTray.getSystemTray().remove(trayIcon);
            }
        }, 3000);
    }

    public static void send(Notification notification) {
        if (notification.message == null || notification.message.isEmpty()) {
            System.err.println("Notification message is empty.");
            return;
        }
        notification.trayIcon.displayMessage("SudokuSolver", notification.message, TrayIcon.MessageType.INFO);
        notification.close();
    }

}
